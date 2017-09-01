<?php

error_reporting(E_ALL | E_STRICT);
set_error_handler(create_function('$nr, $msg = "Error"', 'throw new Exception($msg);'), E_ALL);

require_once("endpoint_compatibility.php");
require_once("endpoint_game.php");

function apiEntry()
{
	if(!isset($_GET["endpoint"]))
	{
		throw new Exception("An endpoint name must be provided.");
	}
	
	$endpointName = $_GET["endpoint"];
	
	switch($endpointName)
	{
	case "compatibility":
		$endpoint = new Endpoint_Compatibility();
		break;
	case "game":
		$endpoint = new Endpoint_Game();
		break;
	default:
		throw new Exception("Unknown endpoint '" . $endpoint . "'.");
	}
	
	return $endpoint->execute();
}

header("Access-Control-Allow-Origin: *");
if($_SERVER['REQUEST_METHOD'] === "OPTIONS")
{
	header("Access-Control-Allow-Methods: GET, POST");
	header("Access-Control-Allow-Headers: Content-Type");
}
else
{
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
		http_response_code(400);
		echo json_encode($error);
	}
}

?>
