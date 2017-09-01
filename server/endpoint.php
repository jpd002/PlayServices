<?php

class Endpoint
{
	function getParam($params, $paramName)
	{
		if(!isset($params[$paramName]))
		{
			throw new Exception(sprintf("%s must be provided.", $paramName));
		}
		return $params[$paramName];
	}
	
	function executeGet()
	{
		throw new Exception("Unsupported method GET.");
	}
	
	function executePost()
	{
		throw new Exception("Unsupported method POST.");
	}
	
	function execute()
	{
		$requestMethod = $_SERVER['REQUEST_METHOD'];
		switch($requestMethod)
		{
		case "GET":
			return $this->executeGet();
			break;
		case "POST":
			return $this->executePost();
			break;
		default:
			throw new Exception("Unsupported method " . $requestMethod . ".");
			break;
		}
	}
}

?>
