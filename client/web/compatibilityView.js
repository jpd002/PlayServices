'use strict'

angular.module('playApp.compatibilityView', ['ngRoute'])

.config(['$routeProvider', 
	function($routeProvider)
	{
		$routeProvider.when('/compatibilityView',
			{
				templateUrl: 'compatibilityView.html',
				controller:  'compatibilityViewController'
			}
		);
	}]
)

.controller('compatibilityViewController', 
	function($scope, $http) 
	{
		$http(
			{
				method: "GET",
				url: "http://localhost/playservices/server/api.php?endpoint=compatibility&gameId=SLES_502.82"
			}
		).then(
			function mySuccess(response)
			{
				$scope.compatibilityReports = response.data;
			},
			function myError(response)
			{
				//$scope.myWelcome = response.statusText;
			}
		);
	}
);
