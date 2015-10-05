'use strict';

/**
 * @ngdoc function
 * @name authclientApp.controller:RefreshCtrl
 * @description
 * # RefreshCtrl
 * Controller of the authclientApp
 */
angular.module('quangauthwebApp')
  .controller('RefreshCtrl', ['$scope', '$location', 'authService', function ($scope, $location, authService) {
      $scope.authentication = authService.authentication;
      $scope.tokenRefreshed = false;
      $scope.tokenResponse = null;

      $scope.refreshToken = function () {

          authService.refreshToken().then(function (response) {
              $scope.tokenRefreshed = true;
              $scope.tokenResponse = response;
          },
           function (err) {
               $location.path('/login');
           });
      };
  }]);
