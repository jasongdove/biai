package biai

import cats.effect.{ExitCode, IO, IOApp}
import pureconfig._
import pureconfig.generic.auto._

import scala.concurrent.duration.DurationInt

object Main extends IOApp {
  override def run(args: List[String]): IO[ExitCode] = {
    val configFolder =
      sys.env.getOrElse("BIAI_CONFIG_FOLDER", s"${System.getProperty("user.home")}/.config/biai")

    val config = ConfigSource.file(s"$configFolder/biai.conf").load[AppConfig]
    config match {
      case Left(failures) =>
        println(failures)
        IO.pure(ExitCode.Error)
      case Right(config) =>
        val cameras = config.cameras
        cameras.foreach(c => println(s"Camera ${c.name} relevant objects: ${c.relevantObjects.mkString(", ")}"))

        val watcher: FolderWatcher = FolderWatcherImpl
        watcher.watch(config.targetFolder, newFile => {
          Image(newFile) match {
            case Some(image) =>
              cameras.find(_.name == image.cameraName) match {
                case Some(camera) =>
                  println(image)
                case None =>
                  println(s"Could not match camera for image at $newFile")
              }
            case None =>
              println(s"Unexpected image file name $newFile")
          }
        })

        IO.sleep(30.seconds) *> IO.pure(ExitCode.Success)
    }
  }
}