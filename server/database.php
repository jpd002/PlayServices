<?php

error_reporting(E_ALL | E_STRICT);

include("config.php");

mysqli_report(MYSQLI_REPORT_STRICT);

class Database
{
	private $connection;
	
	function __construct()
	{
		global $db_server;
		global $db_username;
		global $db_password;
		global $db_database;
		
		$this->connection = new mysqli($db_server, $db_username, $db_password);
		$this->connection->select_db($db_database);
		$this->connection->query("SET NAMES 'utf8'");
	}
	
	function GetGameFromId($id)
	{
		$statement = $this->connection->prepare("SELECT * FROM ps_games WHERE id = ?");
		if(!$statement)
		{
			throw new Exception(__FUNCTION__ . " failed: " . $this->connection->error);
		}
		try
		{
			$statement->bind_param('s', $id);
			if(!$statement->execute())
			{
				throw new Exception(__FUNCTION__ . " failed: " . $this->connection->error);
			}
			
			$result = $statement->get_result();
			if(!$result)
			{
				throw new Exception(__FUNCTION__ . " failed: " . $this->connection->error);
			}
			
			$row = $result->fetch_assoc();
			return $row;
		}
		finally
		{
			$statement->close();
		}
	}
	
	function GetCompatibility($gameId)
	{
		$statement = $this->connection->prepare("SELECT * FROM ps_compatibility WHERE gameId = ?");
		if(!$statement)
		{
			throw new Exception(__FUNCTION__ . " failed: " . $this->connection->error);
		}
		try
		{
			$statement->bind_param('s', $gameId);
			if(!$statement->execute())
			{
				throw new Exception(__FUNCTION__ . " failed: " . $this->connection->error);
			}
			
			$result = $statement->get_result();
			if(!$result)
			{
				throw new Exception(__FUNCTION__ . " failed: " . $this->connection->error);
			}
			
			return $result->fetch_all(MYSQLI_ASSOC);
		}
		finally
		{
			$statement->close();
		}
	}
	
	function InsertCompatibility($gameId, $rating, $deviceInfo)
	{
		$statement = $this->connection->prepare("INSERT INTO ps_compatibility (gameId, rating, deviceInfo) VALUES (?, ?, ?)");
		if(!$statement)
		{
			throw new Exception(__FUNCTION__ . " failed: " . $this->connection->error);
		}
		try
		{
			$statement->bind_param('sis', $gameId, $rating, $deviceInfo);
			if(!$statement->execute())
			{
				throw new Exception(__FUNCTION__ . " failed: " . $this->connection->error);
			}
		}
		finally
		{
			$statement->close();
		}
	}
};

?>
