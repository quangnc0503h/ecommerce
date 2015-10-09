'use strict';

/**
 * @ngdoc overview
 * @name quangauthwebApp
 * @description
 * # quangauthwebApp
 *
 * Main module of the application.
 */
var app = angular
    .module('quangauthwebApp', [
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
        'xdLocalStorage',
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
  });
  //
  app.constant('ngAuthSettings', {
      clientId: 'ngAuthApp'
  });
// day http header len api de check authentication
  app.config(function ($httpProvider) {
      $httpProvider.interceptors.push('authInterceptorService');
  });

  app.run(['$rootScope', '$interval', 'localDataService', 'xdLocalStorage', 'authService', function ($rootScope, $interval, localDataService, xdLocalStorage, authService) {
      localDataService.init();
      $interval(function () {
          if ($rootScope.isAuthLoaded) {
              xdLocalStorage.getItem('xd.authorization').then(function (data) {
                  var authData;
                  if (data.value) {
                      try {
                          authData = JSON.parse(data.value);
                      } catch (err) {
                          //not our message, can ignore
                      }
                  }
                  if (authService.authentication.isAuth) {
                      if (!authData || authData.token != authService.token()) {
                          window.location.reload(true);
                      }
                  } else {
                      if (authData && authData.token) {
                          window.location.reload(true);
                      }
                  }
              });
          }
      }, 3000);
  }]);