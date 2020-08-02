package biai

import pureconfig.ConfigReader
import pureconfig.generic.semiauto.deriveReader

case class CameraConfig(name: String, relevantObjects: List[String])

object CameraConfig {
  implicit val configReader: ConfigReader[CameraConfig] = deriveReader[CameraConfig]
}

case class AppConfig(targetFolder: String, cameras: List[CameraConfig], deepStackEndpoint: String)

object AppConfig {
  implicit val configReader: ConfigReader[AppConfig] = deriveReader[AppConfig]
}