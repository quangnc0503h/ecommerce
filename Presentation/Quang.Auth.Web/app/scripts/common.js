'use strict';

angular.module('quangauthwebApp')
    .directive('showIfAuthenticated', ['$rootScope', 'authService', function ($rootScope, authService) {
        var render = function ($scope, element, attrs, ctrl) {
            $rootScope.$watch("isAuthLoaded", function () {
                if ($rootScope.isAuthLoaded == true) {
                    if (attrs.showIfAuthenticated == 'false' || attrs.showIfAuthenticated === false || attrs.showIfAuthenticated === '0') {
                        //if (authService.authentication.isAuth) {
                        //    element.hide();
                        //} else {
                        //    element.show();
                        //}
                        element.show();
                    } else if (attrs.showIfAuthenticated == 'true' || attrs.showIfAuthenticated === true || attrs.showIfAuthenticated === '1') {
                        if (authService.authentication.isAuth) {
                            element.show();
                        } else {
                            element.hide();
                        }
                    } else {
                        if (authService.authentication.isAuth) {
                            var isAccess = false;
                            var allowRoles = attrs.showIfAuthenticated.split('|');
                            var userRoles = authService.authentication.roles ? authService.authentication.roles : [];
                            for (var j = 0; j < allowRoles.length; j++) {
                                for (var i = 0; i < userRoles.length; i++) {
                                    if (userRoles[i] == allowRoles[j]) {
                                        isAccess = true;
                                        break;
                                    }
                                }
                                if (isAccess) {
                                    break;
                                }
                            }
                            if (isAccess) {
                                //element.toggleClass('ng-hide', false);
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

angular.module('quangauthwebApp')
    .directive('authData', ['$rootScope', '$parse', 'authService', function ($rootScope, $parse, authService) {
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
angular.module('quangauthwebApp')
    .directive('authInitializing', ['$route', '$rootScope', function ($route, $rootScope) {
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
angular.module('quangauthwebApp')
    .directive('menuActiveUrl', ['$route', '$location', function ($route, $location) {

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

angular.module('quangauthwebApp')
  .filter('isActiveText', function () {
      return function (text, activeText, inActiveText) {
          if (text) {
              return typeof activeText != 'undefined' ? activeText : 'Có hoạt động';
          }
          return typeof inActiveText != 'undefined' ? inActiveText : 'Không hoạt động';
      }
  });

angular.module('quangauthwebApp')
  .service('localDataService', ['localStorageService', 'ENV', function (localStorageService, ENV) {
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
