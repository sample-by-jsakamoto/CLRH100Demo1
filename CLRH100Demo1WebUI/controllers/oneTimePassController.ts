namespace CLRH100Demo1WebUI {
    export class OneTimePassController {

        public password: string;

        public counting: boolean;

        public inCounting: boolean;

        public get connected(): boolean {
            return this.mainHub.connectionState == SignalRConnState.Connecting ||
                this.mainHub.connectionState == SignalRConnState.Connected;
        }

        constructor(private $timeout: ng.ITimeoutService, private $rootScope: ng.IRootScopeService, private mainHub: MainHubService) {
            this.counting = true;
            this.inCounting = false;
            this.password = '';

            $rootScope.$on('connectedToHub', () => this.requestOneTimePass());
            if (mainHub.connectionState == SignalRConnState.Connected) {
                this.requestOneTimePass();
            }
        }

        public requestOneTimePass(): void {
            this.counting = true;

            this.mainHub.requestOneTimePass()
                .then(pass => {
                    this.password = pass;
                    this.inCounting = true

                    this.$timeout(() => {
                        this.counting = false;
                        this.inCounting = false;
                        this.$timeout(() => this.requestOneTimePass(), 200);
                    }, 30000);
                });
        }

    }
    angular.module('CLRH100Demo1').controller('oneTimePassController', ['$timeout', '$rootScope', 'mainHub', OneTimePassController]);
}