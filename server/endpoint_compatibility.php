<?php

require_once("database.php");

function endPoint_compatibility()
{
	$requestMethod = $_SERVER['REQUEST_METHOD'];
	if($requestMethod != "POST")
	{
		throw new Exception("Unsupported method " . $requestMethod . ".");
	}
	
	if(!isset($_POST["gameId"]))
	{
		throw new Exception("gameId must be provided.");
	}
	
	if(!isset($_POST["rating"]))
	{
		throw new Exception("rating must be provided.");
	}
	
	$gameId = $_POST["gameId"];
	$rating = $_POST["rating"];
	
	$database = new Database();
	$database->InsertCompatibility($gameId, $rating);
	
	return array("result" => "ok");
}

?>
