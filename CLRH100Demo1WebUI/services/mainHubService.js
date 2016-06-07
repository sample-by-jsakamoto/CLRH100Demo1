var CLRH100Demo1WebUI;
(function (CLRH100Demo1WebUI) {
    var MainHubService = (function () {
        function MainHubService($q, $rootScope) {
            var _this = this;
            this.$q = $q;
            this.$rootScope = $rootScope;
            this.connectionState = 0 /* Connecting */;
            var conn = $.hubConnection();
            this.hub = conn.createHubProxy("MainHub");
            this.hub.on('updatedHostState', function (state) { return _this.onUpdatedHostState(state); });
            // SiganlR 切断時の再接続処理を配線
            var timerState = { id: null };
            conn.stateChanged(function (args) {
                $rootScope.$apply(function () { return _this.connectionState = args.newState; });
                if (args.newState == 1 /* Connected */) {
                    $rootScope.$emit('connectedToHub');
                }
                if (args.newState == 4 /* Disconnected */) {
                    if (timerState.id == null) {
                        timerState.id = setInterval(function () { return conn.start(); }, 5000);
                    }
                }
                else if (timerState.id != null) {
                    clearInterval(timerState.id);
                    timerState.id = null;
                }
            });
            var settings = {};
            if (/\.ngrok\.io$/.test(location.host))
                settings.transport = 'longPolling';
            conn
                .start(settings)
                .then(function () { return $rootScope.$apply(function () {
                $rootScope.$emit('connectedToHub');
            }); });
        }
        MainHubService.prototype.requestCurrentState = function () {
            this.hub.invoke('RequestCurrentState');
        };
        MainHubService.prototype.requestUnlock = function (password) {
            this.hub.invoke('RequestUnlock', password);
        };
        MainHubService.prototype.requestOneTimePass = function () {
            var defer = this.$q.defer();
            this.hub.invoke('RequestOneTimePass').done(function (pass) { return defer.resolve(pass); });
            return defer.promise;
        };
        MainHubService.prototype.onUpdatedHostState = function (state) {
            var _this = this;
            this.$rootScope.$apply(function () { return _this.$rootScope.$emit('updatedHostState', state); });
        };
        return MainHubService;
    }());
    CLRH100Demo1WebUI.MainHubService = MainHubService;
    angular.module('CLRH100Demo1.Services', [])
        .service('mainHub', ['$q', '$rootScope', MainHubService]);
})(CLRH100Demo1WebUI || (CLRH100Demo1WebUI = {}));
//# sourceMappingURL=mainHubService.js.map