'use strict';

/**
 * @ngdoc function
 * @name authclientApp.controller:UnauthorizedCtrl
 * @description
 * # UnauthorizedCtrl
 * Controller of the authclientApp
 */
angular.module('quangauthwebApp')
.controller('UnauthorizedCtrl', ['$scope', '$location', '$modalInstance', 'Message', 'MessageDetail', function ($scope, $location, $modalInstance, Message, MessageDetail) {
    $scope.Message = Message;
    $scope.MessageDetail = MessageDetail;
    $scope.closeAndGotoLogin = function () {
        $modalInstance.close(true);
    }
    $scope.close = function () {
        $modalInstance.close(false);
    }
}]);
