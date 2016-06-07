namespace CLRH100Demo1WebUI {

    /** SignalR の接続状態を示す定数です。 */
    export const enum SignalRConnState {
        Connecting = 0,
        Connected = 1,
        Reconnectiong = 2,
        Disconnected = 4
    }

    export class MainHubService {

        private hub: HubProxy;

        /** サーバー(SignalR)との接続状態 */
        public connectionState: SignalRConnState;

        constructor(private $q: ng.IQService, private $rootScope: ng.IRootScopeService) {
            this.connectionState = SignalRConnState.Connecting;

            var conn = $.hubConnection();
            this.hub = conn.createHubProxy("MainHub");
            this.hub.on('updatedHostState', (state) => this.onUpdatedHostState(state));

            // SiganlR 切断時の再接続処理を配線
            var timerState = { id: null as number };
            conn.stateChanged(args => {
                $rootScope.$apply(() => this.connectionState = args.newState);

                if (args.newState == SignalRConnState.Connected) {
                    $rootScope.$emit('connectedToHub');
                }
                if (args.newState == SignalRConnState.Disconnected) {
                    if (timerState.id == null) {
                        timerState.id = setInterval(() => conn.start(), 5000);
                    }
                }
                else if (timerState.id != null) {
                    clearInterval(timerState.id);
                    timerState.id = null;
                }
            });

            var settings: any = {};
            if (/\.ngrok\.io$/.test(location.host)) settings.transport = 'longPolling';
            conn
                .start(settings)
                .then(() => $rootScope.$apply(() => {
                    $rootScope.$emit('connectedToHub');
                }));
        }

        public requestCurrentState(): void {
            this.hub.invoke('RequestCurrentState');
        }

        public requestUnlock(password: string): void {
            this.hub.invoke('RequestUnlock', password);
        }

        public requestOneTimePass(): ng.IPromise<string> {
            let defer = this.$q.defer<string>()
            this.hub.invoke('RequestOneTimePass').done(pass => defer.resolve(pass));
            return defer.promise;
        }

        private onUpdatedHostState(state: HostState): void {
            this.$rootScope.$apply(() => this.$rootScope.$emit('updatedHostState', state));
        }
    }

    angular.module('CLRH100Demo1.Services', [])
        .service('mainHub', ['$q', '$rootScope', MainHubService]);
}