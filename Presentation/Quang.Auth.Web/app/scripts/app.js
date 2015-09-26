'use strict';

/**
 * @ngdoc overview
 * @name quangauthwebApp
 * @description
 * # quangauthwebApp
 *
 * Main module of the application.
 */
var app = angular.module('quangauthwebApp', ['config',
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
     'xdLocalStorage', 'auth',
        'angular-loading-bar'
]);
app.config(function ($routeProvider, $authProvider) {
    $routeProvider
      .when('/', {
        templateUrl: 'views/main.html',
        controller: 'MainCtrl',
        controllerAs: 'main'
      })
      .when('/about', {
        templateUrl: 'views/about.html',
        controller: 'AboutCtrl',
        controllerAs: 'about'
      })
      .otherwise({
        redirectTo: '/'
      });
    $routeProvider.when("/group", {
        controller: "GroupCtrl",
        templateUrl: "views/group.html",
       resolve: $authProvider.routeResolve()
    });

  });
  
  app.constant('ngAuthSettings', {
      clientId: 'ngAuthApp'
  });

  //app.config(function ($httpProvider) {
  //    $httpProvider.interceptors.push('authInterceptorService');
  //});

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