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
                            xdLocalStorage.getItem('cscode', true).then(function (cscode) {
                                if (cscode.value) {
                                    cscode = authService.textDecode(cscode.value);
                                    var authData;
                                    if (data.value) {
                                        try {
                                            authData = JSON.parse(data.value);
                                        } catch (err) {
                                            //not our message, can ignore
                                        }
                                    }
                                    if (authData && authData.token) {
                                        authService.fillAuthData(authData, cscode + authData.token);
                                    }
                                    $rootScope.isAuthLoaded = true;
                                    defer.resolve(authService.authentication);
                                } else {
                                    xdLocalStorage.removeItem('xd.authorization').then(function () {
                                        $rootScope.isAuthLoaded = true;
                                        defer.resolve({});
                                    }, function () {
                                        $rootScope.isAuthLoaded = true;
                                        defer.resolve({});
                                    });
                                }
                            }, function () {
                                xdLocalStorage.removeItem('xd.authorization').then(function () {
                                    $rootScope.isAuthLoaded = true;
                                    defer.resolve({});
                                }, function () {
                                    $rootScope.isAuthLoaded = true;
                                    defer.resolve({});
                                });
                            });
                        }, function (err) {
                            $rootScope.isAuthLoaded = true;
                            defer.resolve({});
                        });
                    }, function (err) {
                        $rootScope.isAuthLoaded = true;
                        defer.resolve({});
                    });
                    return defer.promise;
                }
            }];
        }
        this.$get = function () { return {}; };
    };
})(window, window.angular);

