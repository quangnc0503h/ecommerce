'use strict';

/**
 * @ngdoc function
 * @name danhmucApp.controller:TokensmanagerCtrl
 * @description
 * # TokensmanagerCtrl
 * Controller of the danhmucApp
 */
angular.module('quangcatewebApp')
  .controller('TokensmanagerCtrl', ['$scope', 'tockensmanagerService', function ($scope, tokensManagerService) {
      $scope.refreshTokens = [];

      tokensManagerService.getRefreshTokens().then(function (results) {

          $scope.refreshTokens = results.data;

      }, function (error) {
          alert(error.data.message);
      });

      $scope.deleteRefreshTokens = function (index, tokenid) {

          tokenid = window.encodeURIComponent(tokenid);

          tokensManagerService.deleteRefreshTokens(tokenid).then(function (results) {

              $scope.refreshTokens.splice(index, 1);

          }, function (error) {
              alert(error.data.message);
          });
      }
  }]);
