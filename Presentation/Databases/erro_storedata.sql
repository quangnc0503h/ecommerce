/*
SQLyog Community v12.09 (64 bit)
MySQL - 10.0.17-MariaDB-wsrep : Database - ErrorStore
*********************************************************************
*/

/*!40101 SET NAMES utf8 */;

/*!40101 SET SQL_MODE=''*/;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
CREATE DATABASE /*!32312 IF NOT EXISTS*/`ErrorStore` /*!40100 DEFAULT CHARACTER SET utf8 */;

USE `ErrorStore`;

/*Table structure for table `Exceptions` */

DROP TABLE IF EXISTS `Exceptions`;

CREATE TABLE `Exceptions` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `GUID` char(36) NOT NULL,
  `ApplicationName` varchar(50) NOT NULL,
  `MachineName` varchar(50) NOT NULL,
  `CreationDate` datetime NOT NULL,
  `Type` varchar(100) NOT NULL,
  `IsProtected` tinyint(1) NOT NULL DEFAULT '0',
  `Host` varchar(100) DEFAULT NULL,
  `Url` varchar(500) DEFAULT NULL,
  `HTTPMethod` varchar(10) DEFAULT NULL,
  `IPAddress` varchar(40) DEFAULT NULL,
  `Source` varchar(100) DEFAULT NULL,
  `Message` varchar(1000) DEFAULT NULL,
  `Detail` mediumtext,
  `StatusCode` int(11) DEFAULT NULL,
  `SQL` mediumtext,
  `DeletionDate` datetime DEFAULT NULL,
  `FullJson` mediumtext,
  `ErrorHash` int(11) DEFAULT NULL,
  `DuplicateCount` int(11) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  KEY `IX_Exceptions_GUID_ApplicationName_DeletionDate_CreationDate` (`GUID`,`ApplicationName`,`DeletionDate`,`CreationDate`),
  KEY `IX_Exceptions_ErrorHash_AppName_CreationDate_DelDate` (`ErrorHash`,`ApplicationName`,`CreationDate`,`DeletionDate`)
) ENGINE=InnoDB AUTO_INCREMENT=7653 DEFAULT CHARSET=utf8;

/*Data for the table `Exceptions` */

LOCK TABLES `Exceptions` WRITE;


UNLOCK TABLES;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
