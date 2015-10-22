'use strict';

/**
 * @ngdoc overview
 * @name quangcatewebApp
 * @description
 * # quangcatewebApp
 *
 * Main module of the application.
 */
var app = angular
  .module('quangcatewebApp', [
    'ngAnimate',
    'ngAria',
    'ngCookies',
    'ngMessages',
    'ngResource',
    'ngRoute',
    'ngSanitize',
    'ngTouch',
    'ui.bootstrap',
    'LocalStorageModule',
     'xdLocalStorage',     
     'angular-loading-bar','auth'
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
    $routeProvider.when("/signup", {
        controller: "SignupCtrl",
        templateUrl: "views/signup.html",
        resolve: $authProvider.routeResolve()
    });



    $routeProvider.when("/refresh", {
        controller: "RefreshCtrl",
        templateUrl: "views/refresh.html",
        resolve: $authProvider.routeResolve()
    });

    $routeProvider.when("/tokens", {
        controller: "TokensmanagerCtrl",
        templateUrl: "views/tokens.html",
        resolve: $authProvider.routeResolve()
    });
    $routeProvider.when("/logout", {
        controller: "LogoutCtrl",
        templateUrl: "views/logout.html",
        resolve: $authProvider.routeResolve()
    });

    $routeProvider.when("/language", {
        controller: "LanguageCtrl",
        templateUrl: "views/language.html",
        resolve: $authProvider.routeResolve()
    });
});

app.constant('ngAuthSettings', {
    clientId: 'ngAuthApp'
});

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
