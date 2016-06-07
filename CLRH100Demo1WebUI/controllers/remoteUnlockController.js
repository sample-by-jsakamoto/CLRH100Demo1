var CLRH100Demo1WebUI;
(function (CLRH100Demo1WebUI) {
    var RemoteUnlockController = (function () {
        function RemoteUnlockController($rootScope, mainHub) {
            var _this = this;
            this.$rootScope = $rootScope;
            this.mainHub = mainHub;
            this.isActive = false;
            this.password = '';
            $rootScope.$on('requestRemoteUnlock', function () { return _this.onRequestRemoteUnlock(); });
        }
        Object.defineProperty(RemoteUnlockController.prototype, "enterEnabled", {
            get: function () { return this.password.length == 4; },
            enumerable: true,
            configurable: true
        });
        RemoteUnlockController.prototype.onRequestRemoteUnlock = function () {
            this.password = '';
            this.isActive = true;
        };
        RemoteUnlockController.prototype.onClickCancel = function () {
            this.password = '';
            this.isActive = false;
        };
        RemoteUnlockController.prototype.onClickNum = function (num) {
            this.password += num.toString();
            this.password = this.password.substring(0, 4);
        };
        RemoteUnlockController.prototype.onClickBackSpace = function () {
            this.password = this.password.substring(0, this.password.length - 1);
        };
        RemoteUnlockController.prototype.onClickEnter = function () {
            if (this.enterEnabled) {
                this.isActive = false;
                this.mainHub.requestUnlock(this.password);
            }
        };
        return RemoteUnlockController;
    }());
    CLRH100Demo1WebUI.RemoteUnlockController = RemoteUnlockController;
    angular.module('CLRH100Demo1').controller('remoteUnlockController', ['$rootScope', 'mainHub', RemoteUnlockController]);
})(CLRH100Demo1WebUI || (CLRH100Demo1WebUI = {}));
//# sourceMappingURL=remoteUnlockController.js.map