'use strict';

var app = angular.module('authclientApp');
app.directive('showIfAuthenticated', ['$rootScope', 'authService', function ($rootScope, authService) {
    var render = function ($scope, element, attrs, ctrl) {
        $rootScope.$watch("isAuthLoaded", function () {
            if ($rootScope.isAuthLoaded == true) {
                if (attrs.showIfAuthenticated == 'false' || attrs.showIfAuthenticated === false || attrs.showIfAuthenticated === '0') {
                    if (authService.authentication.isAuth) {
                        element.hide();
                    } else {
                        element.show();
                    }
                } else if (attrs.showIfAuthenticated == 'true' || attrs.showIfAuthenticated === true || attrs.showIfAuthenticated === '1') {
                    if (authService.authentication.isAuth) {
                        element.show();
                    } else {
                        element.hide();
                    }
                } else {
                    if (authService.authentication.isAuth) {
                        var re;
                        var isAccess = false;
                        var allowRoles = attrs.showIfAuthenticated.split('|');
                        var userRoles = authService.authentication.roles ? authService.authentication.roles : [];
                        for (var j = 0; j < allowRoles.length; j++) {
                            var re = false;
                            if (allowRoles[j] && allowRoles[j].length > 1 && allowRoles[j][0] == '/' && allowRoles[j][allowRoles[j].length - 1] == '/') {
                                re = new RegExp(allowRoles[j].substring(1, allowRoles[j].length - 1));
                            }
                            for (var i = 0; i < userRoles.length; i++) {
                                if (re) {
                                    if (re.test(userRoles[i])) {
                                        isAccess = true;
                                        break;
                                    }
                                } else {
                                    if (userRoles[i] == allowRoles[j]) {
                                        isAccess = true;
                                        break;
                                    }
                                }
                            }
                            if (isAccess) {
                                break;
                            }
                        }
                        if (isAccess) {                            
                            element.show();
                        } else {
                            //element.toggleClass('ng-hide', true);
                            element.hide();
                        }
                    } else {
                        //element.toggleClass('ng-hide', true);
                        element.hide();
                    }
                }
            }
        });
        element.hide();
        //element.toggleClass('ng-hide', true);
    };
    return {
        //require: 'ngModel',
        link: function ($scope, element, attrs, ctrl) {

            render($scope, element, attrs, ctrl);
            // When change content by $location.path()
            $scope.$on('$viewContentLoaded', function () {
                render($scope, element, attrs, ctrl);
            });
        }
    }
}]);

app.directive('authData', ['$rootScope', '$parse', 'authService', function ($rootScope, $parse, authService) {
    var render = function ($scope, element, attrs) {
        $rootScope.$watch("isAuthLoaded", function () {
            if ($rootScope.isAuthLoaded == true) {
                if (authService.authentication.isAuth) {
                    var value = $parse('authentication.' + attrs.authData)(authService);
                    angular.element(element).text(value);
                    element.show();
                } else {
                    element.hide();
                }
                //element.toggleClass('ng-hide', !authService.authentication.isAuth);
            }
        });
        element.hide();
    };
    return {
        restrict: 'A',
        link: function ($scope, element, attrs, ctrl) {

            render($scope, element, attrs, ctrl);
            // When change content by $location.path()
            $scope.$on('$viewContentLoaded', function () {
                render($scope, element, attrs, ctrl);
            });
        }
    }
}]);

app.directive('authInitializing', ['$route', '$rootScope', function ($route, $rootScope) {
    return {
        restrict: 'C',
        link: function ($scope, element, attrs, ctrl) {
            $rootScope.$on('$routeChangeStart', function () {
                element.show();
                element.text('Loading....');
            });
            $rootScope.$on('$routeChangeSuccess', function () {
                element.hide();
                element.text('');
            });
        }
    }
}]);

app.directive('menuActiveUrl', ['$route', '$location', function ($route, $location) {
    return {
        //require: 'ngModel',
        link: function ($scope, element, attrs, ctrl) {
            //alert(attrs.menuActiveClass);
            //alert($route.current);
            //alert($location.path());
            //alert(angular.element("a", element));

            //var elementPath = angular.element("a", element).attr('ng-href').substring(1); alert(elementPath);
            /*
            $scope.$watch('$location.path()', function (locationPath) {
                alert(12);
                //(elementPath === locationPath) ? element.addClass("active") : element.removeClass("active");
            });
            */

            $scope.$on('$viewContentLoaded', function () {
                var urls = [];
                var active = false;
                if (attrs.menuActiveUrl !== '') {
                    urls = attrs.menuActiveUrl.split(',');
                }
                angular.forEach(urls, function (urlPattern) {
                    if (!active && urlPattern != '') {
                        var p = new RegExp("^\/" + urlPattern, 'g');
                        if (p.test($location.path())) {
                            active = true;
                        }
                    }
                });
                //alert(active);
                element.toggleClass('active', active);
            });

        }
    }
}]);

app.service('localDataService', ['localStorageService', 'ENV', function (localStorageService, ENV) {
    var keyPrefix = 'ldv_' + ENV.version + '_';
    var serviceFactory = {};

    // init
    serviceFactory.init = function () {
        // Init to delete local data for old version
        angular.forEach(localStorageService.keys(), function (value, key) {
            var pattern = /^v_(\d+\.\d+\.\d+)_(.*)/g;
            var match = pattern.exec(value);
            if (match) {
                if (match[1] != ENV.version) {
                    localStorageService.remove(value);
                }
            }
        });
    };

    // Wrapper to set
    serviceFactory.set = function (key, value, skipVerion) {
        if (skipVerion) {
            return localStorageService.set(key, value);
        } else {
            var realKey = keyPrefix + key;
            return localStorageService.set(realKey, value);
        }
    };

    // Wrapper to get
    serviceFactory.get = function (key, skipVerion) {
        if (skipVerion) {
            return localStorageService.get(key);
        } else {
            var realKey = keyPrefix + key;
            return localStorageService.get(realKey);
        }
    };

    return serviceFactory;
}]);

app.service('tmpDataService', ['localDataService', function (localDataService) {

    var t = this;

    this.tmpData = {};

    var serviceFactory = {};

    var compare = function (a, b) {
        if (a.value.expired > b.value.expired)
            return -1;
        if (a.value.expired < b.value.expired)
            return 1;
        return 0;
    }

    // Wrapper to set
    serviceFactory.set = function (key, value, isLocalStorage, expired) {
        var localKey = "tLocalData";
        var localValue;
        if (isLocalStorage) {
            localValue = localDataService.get(localKey);
        } else {
            localValue = t.tmpData;
        }
        if (!localValue) {
            localValue = {};
        }
        if (typeof (expired) == 'undefined') {
            expired = 15; // Default expired after 15 minutes
        }

        var expiredDt = new Date();
        expiredDt.setMinutes(expiredDt.getMinutes() + expired);
        expired = expiredDt.getTime();

        var prefix = "tLocalValue";
        var realKey = prefix + key;
        localValue[realKey] = { value: value, expired: expired };
        if (isLocalStorage) {
            localDataService.set(localKey, localValue);
        } else {
            t.tmpData = localValue;
        }

        window.setTimeout(function () {
            var tmp = [];
            for (var item in localValue) {
                if (typeof (item) == 'string' && item.substr(0, prefix.length) == prefix) {
                    var data = localValue[item];
                    if (data.expired < new Date().getTime()) {
                        delete localValue[item];
                    } else {
                        tmp.push({ key: item, value: data });
                    }
                }
            }

            // Max tmp item is 100;
            var maxItems = 100;
            if (tmp.length > maxItems) {
                tmp.sort(compare);
                for (var i = maxItems; i < tmp.length; i++) {
                    delete localValue[tmp[i].key];
                }
            }
            if (isLocalStorage) {
                localDataService.set(localKey, localValue);
            } else {
                t.tmpData = localValue;
            }
        }, 100);
    };

    // Wrapper to get
    serviceFactory.get = function (key, isLocalStorage) {
        var localValue;
        var localKey = "tLocalData";
        if (isLocalStorage) {
            localValue = localDataService.get(localKey);
        } else {
            localValue = t.tmpData;
        }
        if (localValue) {
            var realKey = "tLocalValue" + key;
            var data = localValue[realKey];
            if (data) {
                if (data.expired >= new Date().getTime()) {
                    return data.value;
                } else {
                    delete localValue[realKey];
                    if (isLocalStorage) {
                        localDataService.set(localKey, localValue);
                    } else {
                        t.tmpData = localValue;
                    }
                }
            }
        }
        return null;
    };

    return serviceFactory;
}]);
app.filter('isActiveText', function () {
    return function (text, activeText, inActiveText) {
        if (text) {
            return typeof activeText != 'undefined' ? activeText : 'Có hoạt động';
        }
        return typeof inActiveText != 'undefined' ? inActiveText : 'Không hoạt động';
    }
});

app.filter('translate', ['translationService', function (translationService) {
    return function () {
        return translationService.translate.apply(null, arguments);
    }
}]);