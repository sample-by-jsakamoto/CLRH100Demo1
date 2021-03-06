﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CLRH100Demo1;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;

static class Program
{
    private static Process _TagtoolProc;

    private static MD5 _MD5;

    /// <summary>現在の開錠状態です。</summary>
    private static HostState State { get; set; } = HostState.Waiting;

    /// <summary>SignalR 接続です。</summary>
    private static HubConnection _HubConn;

    /// <summary>SignalR ハブです。</summary>
    private static IHubProxy _MainHub;

    /// <summary>SignalR 接続が絶たれた際の、5秒周期で再接続を試みるタイマーです。</summary>
    private static System.Timers.Timer _ReconnectTimer;

    private static System.Timers.Timer UnlockTimer { get; set; }

    /// <summary>Ctrl+Cが押されたらセットされるイベントオブジェクトです。</summary>
    private static ManualResetEvent _ExitAppEvent = new ManualResetEvent(initialState: false);

    /// <summary>NLog によるロガーです。</summary>
    private static NLog.Logger Logger { get; } = NLog.LogManager.GetLogger("Logger");

    /// <summary>NLog による、デバッグ用のロガーです。</summary>
    private static NLog.Logger Debug { get; } = NLog.LogManager.GetLogger("Debug");

    private static TinyGPIO Gpio21 { get; set; }

    static void Main(string[] args)
    {
        // Ctrl+C が押されたときのハンドラ
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            _ExitAppEvent.Set();
        };

        try
        {
            using (UnlockTimer = new System.Timers.Timer { Interval = 5000, Enabled = false, AutoReset = false })
            {
                InternalMain();
            }
        }
        catch (Exception exception)
        {
            Logger.Error(exception);
        }
        finally
        {
            if (Gpio21 != null)
            {
                Gpio21.Dispose();
            }
        }
    }

    private static void InternalMain()
    {
        Logger.Trace($"======== LAUNCH CLR/H #100 Demo 1 ========");

        _MD5 = MD5.Create();
        UnlockTimer.Elapsed += UnlockTimer_Elapsed;

        // GPIO の初期化
        Logger.Trace("Initialize GPIO.");
        Gpio21 = TinyGPIO.Export(port: 21, direction: GPIODirection.Out);

        // SignalR Hub 接続の開設
        Logger.Trace("Connect to SignalR Server.");
        _HubConn = new HubConnection(AppSettings.Clrh100demo1.Url, useDefaultUrl: true);
        try
        {
            _HubConn.StateChanged += HubConn_StateChanged;
            _MainHub = _HubConn.CreateHubProxy("MainHub");
            _MainHub.On("requestCurrentState", SendCurrentStateToAll);
            _MainHub.On<bool>("requestRemoteUnlock", isAuthorized => OnRequestRemoteUnlock(isAuthorized));
            _HubConn.Start().Wait();
        }
        catch (Exception err)
        {
            Logger.Error(err);
            if (_HubConn.State == ConnectionState.Disconnected)
                HubConn_StateChanged(new StateChange(ConnectionState.Connecting, ConnectionState.Disconnected));
        }

        // NFC タグのスキャンを別スレッドで開始
        BeginNFCTagPolling();

        // 終了イベント待ち
        Logger.Trace("Begin wait hanlde...");
        _ExitAppEvent.WaitOne();
        Logger.Trace("End wait handle.");

        _HubConn.Dispose();
        if (_TagtoolProc != null && !_TagtoolProc.HasExited) _TagtoolProc.Kill();

        Logger.Trace("Force exit.");
    }

    /// <summary>
    /// SignalR 経由で現在状態を求められたときや、NFCタグ検出時に呼び出され、
    /// 現在の開錠状態を他の SignalR クライアントに通知します。
    /// </summary>
    private static void SendCurrentStateToAll()
    {
        Logger.Trace("Sending current state...");
        try
        {
            _MainHub.Invoke("UpdateHostState", State);
            Logger.Trace("Sent current state.");
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }

    /// <summary>
    /// SignalR 接続状態の変更時に呼び出され、切断されたときは5秒周期の再接続処理を実施します。
    /// </summary>
    private static void HubConn_StateChanged(StateChange args)
    {
        Logger.Trace($"[HubConn StateChanged] - {args.NewState}");
        if (args.NewState == ConnectionState.Connected)
        {
            SendCurrentStateToAll();
        }
        if (args.NewState == ConnectionState.Disconnected)
        {
            _ReconnectTimer = new System.Timers.Timer(5000);
            _ReconnectTimer.Elapsed += (_, __) =>
            {
                Logger.Trace($"[HubConn Reconnect...]");
                _HubConn.Start();
            };
            _ReconnectTimer.Start();
        }
        else
        {
            if (_ReconnectTimer != null)
            {
                _ReconnectTimer.Dispose();
                _ReconnectTimer = null;
            }
        }
    }

    /// <summary>
    /// NFC タグリーダによるスキャン(ポーリング)を、副スレッドで開始します。(開始したら、呼び出し元にすぐに返ります)
    /// </summary>
    private static void BeginNFCTagPolling()
    {
        new Task(() =>
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = "tagtool.py -q -l show",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (_TagtoolProc = Process.Start(startInfo))
            {
                Logger.Trace("Launched Tagtool.");
                _TagtoolProc.OutputDataReceived += TagtoolProc_OutputDataReceived;
                _TagtoolProc.ErrorDataReceived += TagtoolProc_ErrorDataReceived;

                Logger.Trace("Begin OutputReadLine...");
                _TagtoolProc.BeginOutputReadLine();
                _TagtoolProc.BeginErrorReadLine();

                Logger.Trace("Wait for exit of Tagtool in sub-thread...");
                _TagtoolProc.WaitForExit();

                Logger.Trace("Exit Tagtool Process.");
            }
        }).Start();
    }

    private static void TagtoolProc_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        try
        {
            Logger.Trace("Detect tag: " + (e.Data ?? "").Substring(0, 9) + "...");
            Debug.Trace("Raw data  : " + e.Data); // 生の ID はデバッグ時のみ

            var hashedBytes = _MD5.ComputeHash(Encoding.ASCII.GetBytes(e.Data + ":" + AppSettings.Salt));
            var hashedText = string.Join("", hashedBytes.Select(n => n.ToString("x2")));
            Logger.Trace(hashedText);

            // 開錠許可されている、登録済の NFC タグかどうか判定
            var isAuthorized = AppSettings.AuthorizedKeys
                .Split(',')
                .Any(key => key == hashedText);
            Logger.Trace($"IsAuthorized: {isAuthorized}");

            // NFCタグ検出音を再生
            PlayDetectNFCTagSound(isAuthorized);

            if (isAuthorized)
            {
                Logger.Trace("UNLOCK");
                Gpio21.Value = 1;
                SetState(HostState.Unlocked);
            }
            else
            {
                Gpio21.Value = 0;
                SetState(HostState.Rejected);
            }

            UnlockTimer.Stop();
            UnlockTimer.Start();
        }
        catch (Exception exception)
        {
            Logger.Error(exception);
        }
    }

    private static void OnRequestRemoteUnlock(bool isAuthorized)
    {
        // 効果音を再生
        PlayDetectNFCTagSound(isAuthorized);

        if (isAuthorized)
        {
            Logger.Trace("REMOTE UNLOCK");
            Gpio21.Value = 1;
            SetState(HostState.Unlocked);
        }
        else
        {
            SetState(HostState.Rejected);
        }

        UnlockTimer.Stop();
        UnlockTimer.Start();
    }


    private static void SetState(HostState state)
    {
        State = state;
        SendCurrentStateToAll();
    }

    /// <summary>
    /// 開錠期限のタイマーが期限切れになったときに呼び出されます。
    /// </summary>
    private static void UnlockTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        Gpio21.Value = 0;
        Logger.Trace("End of unlock period.");
        SetState(HostState.Waiting);
    }

    private static Process _PlayerProcess;

    /// <summary>
    /// NFCタグ検出音を再生
    /// </summary>
    private static void PlayDetectNFCTagSound(bool isAuthorized)
    {
        try
        {
            if (_PlayerProcess != null && !_PlayerProcess.HasExited)
            {
                _PlayerProcess.Kill();
                Thread.Sleep(100);
            }
            _PlayerProcess = null;
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }

        try
        {
            var wavName = isAuthorized ? "allow.wav" : "deny.wav";
            _PlayerProcess = Process.Start("aplay", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, wavName));
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }

    private static void TagtoolProc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        var text = (e.Data ?? "").Trim(' ', '\t', '\r', '\n');
        if (text == "** waiting for a tag **")
            Logger.Trace(text);
        else if (text != "")
        {
            Logger.Error(text);
        }
    }
}
