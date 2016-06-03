# CLRH100Demo1

## Raspberry Pi へのインストール

### ファイルの配置先

配置先に制約はありません。  
しかし運用管理をやりやすくするために、「**/hom/pi/CLRH100Demo1**」というフォルダを作成して、その中にプログラムの実行ファイル (.exe) や依存関係にある DLL (.dll)、アプリケーション構成ファイル (.config) など一式を格納することにします。

#### 具体的な手順

主たる作業環境 OS であろう Windows OS 上で、ビルド済みバイナリの Zip ファイル (例: `CLRH100Demo1-v.1.0.0.zip`) を入手します。

入手したビルド済みバイナリの Zip ファイルを、scp コマンドを使うなどして、Raspberry Pi のホームディレクトリにコピーします。

```
> pushd (CLRH100Demo1-v.1.0.0.zip があるフォルダ)
> scp CLRH100Demo1-v.1.0.0.zip pi@(Raspberry Pi の IP アドレス):~/
```

ssh で raspberry Pi のシェルに接続し、ホームディレクトリにコピーされた CLRH100Demo1-v.1.0.0.zip を「~/CLRH100Demo1」フォルダに解凍します。

```
> ssh -l pi (Raspberry Pi の IP アドレス)
$ cd ~
$ unzip CLRH100Demo1-v.1.0.0.zip -d ./CLRH100Demo1
```

解凍し終わったら、.exe ファイルに実行可能権限を付与します。次の作業があるので、ssh はいったん抜けます。

```
$ cd ~/CLRH100Demo1
$ chmod +x ./CLRH100Demo1.exe
$ exit
>
```

次に、本番用の設定が書き込まれた custom.appSettings.config を主作業環境にて入手します (Zip圧縮で配布されている場合は、この手順の場合は先に解凍して custom.appSettings.config を適当なフォルダに取り出しておきます)。

入手した custom.appSettings.config を、Raspberry Pi の「~/CLRH100Demo1」フォルダに scp コマンドを使うなどしてコピーします。

```
> pushd (custom.appSettings.config があるフォルダ)
> scp custom.appSettings.config pi@(Raspberry Pi の IP アドレス):~/CLRH100Demo1/
```

ここまでできたら、実行可能です。

#### 試験実行

ssh で Raspberry Pi のシェルに再接続し、プログラムを実行します。

```
> ssh -l pi (Raspberry Pi の IP アドレス)
$ cd ~/CLRH100Demo1
$ ./CLRH100Demo1.exe
```

プログラムを終了するには Ctrl + C を押してください。  
また、ssh が切断されると、本プログラムも終了します。

なお、Ctrl + C 等でプログラム終了後、次回起動時に、NFC タグリーダーとのアクセスでエラーが発生するようになることがあります。  
この場合は、現状、いったん NFC タグリーダーを、USB ポートから抜いて指し直すまで復旧しません。



### 自動起動の設定

以下のコマンドを実行して「30秒の待機後、カレントフォルダを /home/pi/CLRH100Demo1 に移動し、それからCLRH100Demo1.exe を実行する」というシェルスクリプト「/etc/init.d/CLRH100Demo1.sh」を作成します。

```
$ sudo echo \#\!/bin/sh > /etc/init.d/CLRH100Demo1.sh
$ sudo echo sleep 30 >> /etc/init.d/CLRH100Demo1.sh
$ sudo echo cd /home/pi/CLRH100Demo1 >> /etc/init.d/CLRH100Demo1.sh
$ sudo echo ./CLRH100Demo1.exe >> /etc/init.d/CLRH100Demo1.sh
```

> - カレントフォルダを .exe があるフォルダに移動しないと .exe と同じフォルダに置いたカスタムアプリケーション構成 (custom.appSettings.config) を読み込めないようです。
> - 30秒の待機を入れてあるのは、NFCカードリーダーやWiFi接続などの外部環境が安定するのを待つためです。


続けて以下のコマンドを実行して、上記で作成したシェルスクリプト「/etc/init.d/CLRH100Demo1.sh」に実行権限を付与し、さらに自動起動に組み込みます。

```
$ sudo chamod +x /etc/init.d/CLRH100Demo1.sh
$ sudo update-rc.d CLRH100Demo1.sh defaults
```

> 自動起動を解除するには以下のコマンドを実行します。
> ```
> $ sudo update-rc.d -f CLRH100Demo1.sh remove
> ```

### 音量調整について

NFCタグを検出時に音を鳴らすようになっていますが、このとき、Raspberry Pi のイヤホン端子から再生される音量を調整するには、下記コマンドを使います。

```
amixier cset numid=1 100%
```

上記コマンドの「100%」が音量を示すパラメータで、100% が最大音量、0% が無音となります。  


## ログ

CLRH100Demo1.exe があるフォルダに、「trace.txt」という名前のテキストファイルで、トレースログが保存されます。

最大 4MB のサイズを上限として、「trcae.001.txt」というファイル名に循環アーカイブしつつ、書き込みを継続します。

ログ出力の仕組みは NLog によって実装しており、CLRH100Demo1.exe.config 内の記述によって挙動を制御できます。

## 開発時の配置

`set_env.bat` に、配置先 Raspberry Pi の IP アドレスなどしかるべき設定を行っておくと、ビルド時にビルド完了後バッチスクリプトによって、scp でビルド結果の成果物を Raspberry Pi にコピーするようになっています。

コピー先は ~/CLRH100Demo1 です。

## 添付の効果音

[フリー効果音素材 くらげ工匠](www.kurage-kosho.info) の button62.mp3 及び button82.mp3 を使用しています。