package biai

import cats.effect._
import cats.implicits._
import org.http4s.client.blaze.BlazeClientBuilder
import pureconfig._
import pureconfig.module.catseffect.syntax._

import scala.concurrent.ExecutionContext.Implicits.global

object Main extends IOApp {
  override def run(args: List[String]): IO[ExitCode] = {
    val configFolder =
      sys.env.getOrElse("BIAI_CONFIG_FOLDER", s"${System.getProperty("user.home")}/.config/biai")

    val logger: Logger = LoggerImpl

    resources.use {
      case (blocker, httpClient) => {
        for {
          config <- ConfigSource.file(s"$configFolder/biai.conf").loadF[IO, AppConfig](blocker)
          _ <- logCameras(config.cameras, logger)
          deepStackService: DeepStackService = new DeepStackServiceImpl(config, blocker, httpClient, logger)
          imageProcessor: ImageProcessor = new ImageProcessorImpl(deepStackService, logger)
          folderWatcher: FolderWatcher = new FolderWatcherImpl(config, imageProcessor, logger)
          _ <- IO(folderWatcher.start())
          _ <- IO.never
        } yield ExitCode.Success
      }
    }
  }

  private def resources =
    for {
      blocker <- Blocker[IO]
      httpClient <- BlazeClientBuilder[IO](global).resource
    } yield (blocker, httpClient)

  private def logCameras(cameras: List[CameraConfig], logger: Logger): IO[Unit] =
    cameras.map(c => logger.log(s"Camera ${c.name} relevant objects: ${c.relevantObjects.mkString(", ")}")).sequence_
}
