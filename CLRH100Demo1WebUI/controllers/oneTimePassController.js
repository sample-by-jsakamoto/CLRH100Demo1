var CLRH100Demo1WebUI;
(function (CLRH100Demo1WebUI) {
    var OneTimePassController = (function () {
        function OneTimePassController($timeout, $rootScope, mainHub) {
            var _this = this;
            this.$timeout = $timeout;
            this.$rootScope = $rootScope;
            this.mainHub = mainHub;
            this.counting = true;
            this.inCounting = false;
            this.password = '';
            $rootScope.$on('connectedToHub', function () { return _this.requestOneTimePass(); });
            if (mainHub.connectionState == 1 /* Connected */) {
                this.requestOneTimePass();
            }
        }
        Object.defineProperty(OneTimePassController.prototype, "connected", {
            get: function () {
                return this.mainHub.connectionState == 0 /* Connecting */ ||
                    this.mainHub.connectionState == 1 /* Connected */;
            },
            enumerable: true,
            configurable: true
        });
        OneTimePassController.prototype.requestOneTimePass = function () {
            var _this = this;
            this.counting = true;
            this.mainHub.requestOneTimePass()
                .then(function (pass) {
                _this.password = pass;
                _this.inCounting = true;
                _this.$timeout(function () {
                    _this.counting = false;
                    _this.inCounting = false;
                    _this.$timeout(function () { return _this.requestOneTimePass(); }, 200);
                }, 30000);
            });
        };
        return OneTimePassController;
    }());
    CLRH100Demo1WebUI.OneTimePassController = OneTimePassController;
    angular.module('CLRH100Demo1').controller('oneTimePassController', ['$timeout', '$rootScope', 'mainHub', OneTimePassController]);
})(CLRH100Demo1WebUI || (CLRH100Demo1WebUI = {}));
//# sourceMappingURL=oneTimePassController.js.map