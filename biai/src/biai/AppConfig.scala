package biai

case class Camera(name: String, relevantObjects: List[String])

case class AppConfig(targetFolder: String, cameras: List[Camera], deepStackEndpoint: String)
