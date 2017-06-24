<?php

require_once("database.php");

function getParam($params, $paramName)
{
	if(!isset($params[$paramName]))
	{
		throw new Exception(sprintf("%s must be provided.", $paramName));
	}
	return $params[$paramName];
}

function endPoint_compatibility_get()
{
	$gameId = getParam($_GET, "gameId");

	$database = new Database();
	$result = $database->GetCompatibility($gameId);
	
	return $result;
}

function endPoint_compatibility_post()
{
	$rawParams = file_get_contents("php://input");
	$params = json_decode($rawParams, TRUE);
	
	$gameId     = getParam($params, "gameId");
	$rating     = getParam($params, "rating");
	$deviceInfo = getParam($params, "deviceInfo");
	
	$database = new Database();
	$database->InsertCompatibility($gameId, $rating, json_encode($deviceInfo));
	
	return array("result" => "ok");
}

function endPoint_compatibility()
{
	$requestMethod = $_SERVER['REQUEST_METHOD'];
	switch($requestMethod)
	{
	case "GET":
		return endPoint_compatibility_get();
		break;
	case "POST":
		return endPoint_compatibility_post();
		break;
	default:
		throw new Exception("Unsupported method " . $requestMethod . ".");
		break;
	}
}

?>
