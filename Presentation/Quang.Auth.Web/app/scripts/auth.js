(function (window, angular, undefined) {
    'use strict';
    var module = angular.module('auth', ['config']).provider('$auth', $authProvider);
    function $authProvider() {
        var t = this;
        this.routeResolve = function (resolve) {
            if (typeof resolve == 'undefined') {
                resolve = {};
            }
            return angular.extend(Object.create(resolve), {
                authUser: t.loadAuthUser()
            })
        }
        this.loadAuthUser = function () {
            return ['$q', '$injector', '$rootScope', '$timeout', 'xdLocalStorage', 'authService', 'ENV', function ($q, $injector, $rootScope, $timeout, xdLocalStorage, authService, ENV) {
                if ($rootScope.isAuthLoaded) {
                    return authService.authentication;
                } else {
                    var defer = $q.defer();
                    xdLocalStorage.init({
                        iframeUrl: ENV.urlIframeSso
                    }).then(function () {
                        xdLocalStorage.getItem('xd.authorization').then(function (data) {
                            var authData;
                            if (data.value) {
                                try {
                                    authData = JSON.parse(data.value);
                                } catch (err) {
                                    //not our message, can ignore
                                }
                            }
                            if (authData && authData.token) {
                                authService.fillAuthData(authData, authData.token);
                            }
                            defer.resolve(authService.authentication);
                            $rootScope.isAuthLoaded = true;
                        });
                    });
                    return defer.promise;
                }
            }];
        }
        this.$get = function () { return {}; };
    };
})(window, window.angular);

