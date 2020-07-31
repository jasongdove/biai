package biai

import better.files.File
import cats.effect.{ExitCode, IO, IOApp}
import io.methvin.better.files.RecursiveFileMonitor
import pureconfig._
import pureconfig.generic.auto._

import scala.concurrent.ExecutionContext.Implicits.global
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
        val watcher = new RecursiveFileMonitor(File(config.targetFolder)) {
          override def onCreate(file: File, count: Int): Unit =
            println(file.path)
        }
        watcher.start()

        IO.sleep(30.seconds) *> IO.pure(ExitCode.Success)
    }
  }
}