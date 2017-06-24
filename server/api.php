<?php

error_reporting(E_ALL | E_STRICT);

require_once("endpoint_game.php");
require_once("endpoint_compatibility.php");

function apiEntry()
{
	if(!isset($_GET["endpoint"]))
	{
		throw new Exception("An endpoint name must be provided.");
	}
	
	$endpoint = $_GET["endpoint"];
	
	switch($endpoint)
	{
	case "game":
		return endPoint_game();
		break;
	case "compatibility":
		return endPoint_compatibility();
		break;
	default:
		throw new Exception("Unknown endpoint '" . $endpoint . "'.");
		break;
	}
}

header("Content-Type: application/json");
try
{
	$result = apiEntry();
	echo json_encode($result);
}
catch(Exception $exception)
{
	$error = array(
		"error" => 
			array(
				"description" => $exception->getMessage()
			)
		);
	echo json_encode($error);
}

?>
