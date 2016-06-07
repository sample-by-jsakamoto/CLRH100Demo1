namespace CLRH100Demo1WebUI {
    export class MainController {

        public state: HostState;

        public get connected(): boolean {
            return this.mainHub.connectionState == SignalRConnState.Connecting ||
                this.mainHub.connectionState == SignalRConnState.Connected;
        }

        constructor(private $rootScope: ng.IRootScopeService, private mainHub: MainHubService) {
            this.state = HostState.Waiting;

            $rootScope.$on('connectedToHub', () => mainHub.requestCurrentState());
            $rootScope.$on('updatedHostState', (event, state) => this.onUpdatedHostState(state));

            // 初期状態要求をホストに通知
            if (mainHub.connectionState == SignalRConnState.Connected) {
               mainHub.requestCurrentState();
            }
        }

        private onUpdatedHostState(newState: HostState): void {
            this.state = newState;
        }

        public onClickRemoteUnlock(): void {
            this.$rootScope.$emit('requestRemoteUnlock');
        }
    }

    angular.module('CLRH100Demo1').controller('mainController', ['$rootScope', 'mainHub', MainController]);
}