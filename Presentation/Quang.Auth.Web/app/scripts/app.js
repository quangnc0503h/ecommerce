'use strict';

/**
 * @ngdoc overview
 * @name authclientApp
 * @description
 * # authclientApp
 *
 * Main module of the application.
 */
var app = angular
    .module('authclientApp', [
        'config',
        'ngAnimate',
        'ngCookies',
        'ngMessages',
        'ngResource',
        'ngRoute',
        'ngSanitize',
        'ngTouch',
        'ui.bootstrap',
        'LocalStorageModule',
        'xdLocalStorage',"isteven-multi-select",
        'auth',
        'angular-loading-bar'
    ]);
app.config(function ($routeProvider, $authProvider) {
    $routeProvider
       .when('/', {
           templateUrl: 'views/main.html',
           controller: 'MainCtrl',
           resolve: $authProvider.routeResolve()
       })
      .when('/about', {
        templateUrl: 'views/about.html',
        controller: 'AboutCtrl',
        resolve: $authProvider.routeResolve()
      })
      .otherwise({
        redirectTo: '/'
      });
    $routeProvider.when("/login", {
        controller: "LoginCtrl",
        templateUrl: "views/login.html",
        resolve: $authProvider.routeResolve()
    });
    $routeProvider.when("/login/:returnUrl", {
        controller: "LoginCtrl",
        templateUrl: "views/login.html",
        resolve: $authProvider.routeResolve()
    });
    $routeProvider.when("/logout", {
        controller: "LogoutCtrl",
        templateUrl: "views/logout.html",
        resolve: $authProvider.routeResolve()
    });

    $routeProvider.when("/changepass", {
        controller: "ChangepassCtrl",
        templateUrl: "views/changepass.html",
        resolve: $authProvider.routeResolve()
    });

    $routeProvider.when("/signup", {
        controller: "SignupCtrl",
        templateUrl: "views/signup.html",
        resolve: $authProvider.routeResolve()
    });

    $routeProvider.when("/settings", {
        controller: "SettingsCtrl",
        templateUrl: "views/settings.html",
        resolve: $authProvider.routeResolve()
    });

    $routeProvider.when("/tokens", {
        controller: "TokensmanagerCtrl",
        templateUrl: "views/tokens.html",
        resolve: $authProvider.routeResolve()
    });

    $routeProvider.when("/associate", {
        controller: "associateController",
        templateUrl: "views/associate.html",
        resolve: $authProvider.routeResolve()
    });

    $routeProvider.when("/group", {
        controller: "GroupCtrl",
        templateUrl: "views/group.html",
        resolve: $authProvider.routeResolve()
    });

    $routeProvider.when("/group/grant/:id", {
        controller: "GroupGrantCtrl",
        templateUrl: "views/group-grant.html",
        resolve: $authProvider.routeResolve()
    });

    $routeProvider.when("/role", {
        controller: "PermissionCtrl",
        templateUrl: "views/permission.html",
        resolve: $authProvider.routeResolve()
    });
    $routeProvider.when("/role/grant/:id", {
        controller: "PermissionGrantCtrl",
        templateUrl: "views/permission-grant.html",
        resolve: $authProvider.routeResolve()
    });

    $routeProvider.when("/term", {
        controller: "TermCtrl",
        templateUrl: "views/term.html",
        resolve: $authProvider.routeResolve()
    });

    $routeProvider.when("/user", {
        controller: "UserCtrl",
        templateUrl: "views/user.html",
        resolve: $authProvider.routeResolve()
    });

    $routeProvider.when("/group/users/:groupId", {
        controller: "UserCtrl",
        templateUrl: "views/user.html",
        resolve: $authProvider.routeResolve()
    });

    $routeProvider.when("/user/add", {
        controller: "UserEditCtrl",
        templateUrl: "views/user-edit.html",
        resolve: $authProvider.routeResolve()
    });

    $routeProvider.when("/user/edit/:id", {
        controller: "UserEditCtrl",
        templateUrl: "views/user-edit.html",
        resolve: $authProvider.routeResolve()
    });

    $routeProvider.when("/user/grant/:id", {
        controller: "UserGrantCtrl",
        templateUrl: "views/user-grant.html",
        resolve: $authProvider.routeResolve()
    });
    $routeProvider.when("/device", {
        controller: "DeviceCtrl",
        templateUrl: "views/device.html",
        resolve: $authProvider.routeResolve()
    });
    $routeProvider.when("/login-history", {
        controller: "LoginHistoryCtrl",
        templateUrl: "views/login-history.html",
        resolve: $authProvider.routeResolve()
    });
  });
  //
  app.constant('ngAuthSettings', {
      clientId: 'ngAuthApp'
  });
// day http header len api de check authentication
  app.config(function ($httpProvider) {
      $httpProvider.interceptors.push('authInterceptorService');
  });

  app.run(['$rootScope', '$injector', '$interval', 'localDataService', 'xdLocalStorage', 'authService',  'ENV', function ($rootScope, $injector, $interval, localDataService, xdLocalStorage, authService,  ENV) {
      localDataService.init();
      $rootScope.ENV = ENV;
      $interval(function () {
          if ($rootScope.isAuthLoaded) {
              xdLocalStorage.getItem('xd.authorization').then(function (data) {
                  xdLocalStorage.getItem('cscode', true).then(function (cscode) {
                      cscode = authService.textDecode(cscode.value);
                      var authData;
                      if (data.value) {
                          try {
                              authData = JSON.parse(data.value);
                              if (authData.expires) {
                                  var expiredDt = new Date(authData.expires);
                                  var now = new Date();
                                  if (now.getTime() + 60 * 1000 > expiredDt.getTime()) {
                                      authService.logOut().then(function () {
                                          _showModalUnauthorized();
                                      });
                                  }
                              }
                          } catch (err) {
                              //not our message, can ignore
                          }
                      }
                      if (authService.authentication.isAuth) {
                          if (!authData || (cscode + authData.token) != authService.token()) {
                              window.location.reload(true);
                          }
                      } else {
                          if (authData && cscode && authData.token) {
                              window.location.reload(true);
                          }
                      }
                  });
              });
          }
      }, 3000);
      var _showModalUnauthorized = function () {
          var $modal = $injector.get('$modal');
          var $location = $injector.get('$location');
          var modalInstance = $modal.open({
              templateUrl: 'views/unauthorized.html',
              controller: 'UnauthorizedCtrl',
              resolve: {
                  Message: function () {
                      return null;
                  },
                  MessageDetail: function () {
                      return null;
                  }
              },
              backdropClass: 'backdrop-unauthorized'
          });
          modalInstance.result.then(function (shouldLoginRedirect) {
              if (shouldLoginRedirect) {
                  window.location.href = shouldLoginRedirect;
              } else {
                  $location.path('/');
              }
          }, function () {
              $location.path('/login');
          });
      }
  }]);