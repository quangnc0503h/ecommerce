'use strict';

/**
 * @ngdoc function
 * @name authclientApp.controller:ChangepassCtrl
 * @description
 * # ChangepassCtrl
 * Controller of the authclientApp
 */
angular.module('quangauthwebApp')
  .controller('SettingsCtrl', ['$scope', '$location', 'authService', 'userService', 'groupService', function ($scope, $location, authService, userService, groupService) {
      
      $scope.user = {};
      userService.getCurrentUser(function (item) {
          $scope.user = item;
      });

      $scope.updateSettings = function () {
          if ($scope.editForm.$valid) {
              userService.saveCurrentUser($scope.user, function (success) {
                  if (success) {
                      alert('Đã thiết lập thông tin cá nhân thành công');
                  } else {
                      alert('Error: Pls try again!');
                  }
              });
          }
      };
  }]);
