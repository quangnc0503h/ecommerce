'use strict';

/**
 * @ngdoc function
 * @name danhmucApp.controller:RefreshCtrl
 * @description
 * # RefreshCtrl
 * Controller of the danhmucApp
 */
angular.module('quangwebApp')
  .controller('RefreshCtrl',  ['$scope', '$location', 'authService', function ($scope, $location, authService) {
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
