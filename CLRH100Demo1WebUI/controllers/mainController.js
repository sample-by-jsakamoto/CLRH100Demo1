var CLRH100Demo1WebUI;
(function (CLRH100Demo1WebUI) {
    var MainController = (function () {
        function MainController($rootScope, mainHub) {
            var _this = this;
            this.$rootScope = $rootScope;
            this.mainHub = mainHub;
            this.state = 0 /* Waiting */;
            $rootScope.$on('connectedToHub', function () { return mainHub.requestCurrentState(); });
            $rootScope.$on('updatedHostState', function (event, state) { return _this.onUpdatedHostState(state); });
            // 初期状態要求をホストに通知
            if (mainHub.connectionState == 1 /* Connected */) {
                mainHub.requestCurrentState();
            }
        }
        Object.defineProperty(MainController.prototype, "connected", {
            get: function () {
                return this.mainHub.connectionState == 0 /* Connecting */ ||
                    this.mainHub.connectionState == 1 /* Connected */;
            },
            enumerable: true,
            configurable: true
        });
        MainController.prototype.onUpdatedHostState = function (newState) {
            this.state = newState;
        };
        MainController.prototype.onClickRemoteUnlock = function () {
            this.$rootScope.$emit('requestRemoteUnlock');
        };
        return MainController;
    }());
    CLRH100Demo1WebUI.MainController = MainController;
    angular.module('CLRH100Demo1').controller('mainController', ['$rootScope', 'mainHub', MainController]);
})(CLRH100Demo1WebUI || (CLRH100Demo1WebUI = {}));
//# sourceMappingURL=mainController.js.map