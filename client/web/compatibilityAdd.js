'use strict'

angular.module('playApp.compatibilityAdd', ['ngRoute'])

.config(['$routeProvider', 
	function($routeProvider)
	{
		$routeProvider.when('/compatibilityAdd',
			{
				templateUrl: 'compatibilityAdd.html',
				controller:  'compatibilityAddController'
			}
		);
	}]
)

.controller('compatibilityAddController',
	function($scope, $http)
	{
		$scope.postCompat =
			function()
			{
				$http.post(
						'http://localhost/playservices/server/api.php?endpoint=compatibility', 
						{
							gameId: $scope.gameId,
							rating: $scope.rating,
							deviceInfo: 
							{
								osName: 'web'
							}
						}
					)
					.then(
						function()
						{
							
						},
						function(response)
						{
							alert(response.data.error.description);
						}
					);
			};
	}
);
