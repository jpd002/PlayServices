'use strict'

angular.module('playApp', 
	[
		'ngRoute',
		'playApp.compatibilityAdd',
		'playApp.compatibilityView'
	]
)
.config(['$locationProvider', '$routeProvider', 
	function($locationProvider, $routeProvider)
	{
		$locationProvider.hashPrefix('!');
		$routeProvider.otherwise({ redirectTo: '/compatibilityView'});
	}
]);
