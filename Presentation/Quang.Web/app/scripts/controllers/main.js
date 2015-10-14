'use strict';

/**
 * @ngdoc function
 * @name quangwebApp.controller:MainCtrl
 * @description
 * # MainCtrl
 * Controller of the quangwebApp
 */
angular.module('quangwebApp')
  .controller('MainCtrl', ['$rootScope', '$scope', '$location', 'ENV', 'authService',   'xdLocalStorage', 'authUser', function ($rootScope, $scope, $location, ENV, authService,  xdLocalStorage, authUser) {
      $scope.summary = {};
      if (authService.authentication.isAuth) {
       
      }
      //console.log(xdLocalStorage.getItem("url_3438512491993606", !0));
      $scope.login = function () {
          var str = new String(Math.random());
          var id = str.substr(str.indexOf(".") + 1);
         // console.log(id);
          xdLocalStorage.setItem('url_' + id, encodeURIComponent(window.location.href)).then(function () {
              window.location.href = ENV.urlLoginSso + '/' + id;
          });
         // console.log(xdLocalStorage.getItem("url_" + id));
      }

      //$scope.text = translationService.translate('Hello1');
  }]);