namespace CLRH100Demo1WebUI {

    export class RemoteUnlockController {

        public isActive: boolean;

        public password: string;

        public get enterEnabled(): boolean { return this.password.length == 4; }

        constructor(private $rootScope: ng.IRootScopeService, private mainHub: MainHubService) {
            this.isActive = false;
            this.password = '';
            $rootScope.$on('requestRemoteUnlock', () => this.onRequestRemoteUnlock());
        }

        private onRequestRemoteUnlock(): void {
            this.password = '';
            this.isActive = true;
        }

        public onClickCancel(): void {
            this.password = '';
            this.isActive = false;
        }

        public onClickNum(num: number): void {
            this.password += num.toString();
            this.password = this.password.substring(0, 4);
        }

        public onClickBackSpace(): void {
            this.password = this.password.substring(0, this.password.length - 1);
        }

        public onClickEnter(): void {
            if (this.enterEnabled) {
                this.isActive = false;
                this.mainHub.requestUnlock(this.password);
            }
        }
    }

    angular.module('CLRH100Demo1').controller('remoteUnlockController', ['$rootScope', 'mainHub', RemoteUnlockController]);
}