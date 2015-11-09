'use strict';

/**
 * @ngdoc function
 * @name authclientApp.controller:ChangepassCtrl
 * @description
 * # ChangepassCtrl
 * Controller of the authclientApp
 */
angular.module('authclientApp')
  .controller('ChangepassCtrl', ['$scope', '$location', 'authService', 'userService', 'groupService', function ($scope, $location, authService, userService, groupService) {
      
      $scope.item = {};
      $scope.user = {};
      userService.getCurrentUser(function (item) {
          $scope.user = item;
      });

      $scope.changePass = function () {
          if ($scope.editForm.$valid) {
              var fn = $scope.user.HasPassword == true ? userService.changepass : userService.setpassword;
              fn($scope.item, function (success) {
                  if (success) {
                      alert('Đã thực hiện thay đổi mật khẩu thành công');
                      $location.path('/logout');
                  } else {
                      alert('Error: Pls try again!');
                  }
              });
          }
      };
  }]);
