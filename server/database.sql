CREATE TABLE `ps_games` (
  `id` char(11) NOT NULL,
  `title` varchar(255) NOT NULL,
  PRIMARY KEY (`id`)
) DEFAULT CHARSET=utf8

CREATE TABLE `ps_compatibility` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `gameId` char(11) NOT NULL,
  `rating` int(11) NOT NULL,
  `deviceInfo` json NOT NULL,
  PRIMARY KEY (`id`)
) DEFAULT CHARSET=utf8
