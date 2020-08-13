package biai

import cats.effect._
import cats.implicits._
import org.http4s.client.blaze.BlazeClientBuilder
import pureconfig._
import pureconfig.module.catseffect.syntax._

import scala.concurrent.ExecutionContext.Implicits.global

object Main extends IOApp {
  override def run(args: List[String]): IO[ExitCode] = {

    resources
      .use {
        case (imageProcessor, queue, logger) => {
          queue.dequeue
            .evalMap {
              case Left(throwable) => logger.log(throwable.getMessage)
              case Right(file)     => imageProcessor.processImage(file)
            }
            .compile
            .drain
        }
      }
      .as(ExitCode.Success)
  }

  private def resources = {
    val configFolder =
      sys.env.getOrElse("BIAI_CONFIG_FOLDER", s"${System.getProperty("user.home")}/.config/biai")

    val logger: Logger = LoggerImpl

    for {
      blocker <- Blocker[IO]
      config <- ConfigSource.file(s"$configFolder/biai.conf").loadF[IO, AppConfig](blocker).asResource
      _ <- logCameras(config.cameras, logger).asResource
      queue <- fs2.concurrent.Queue.bounded[IO, FolderWatcher.Event](100).asResource
      httpClient <- BlazeClientBuilder[IO](global).resource
      _ <- FolderWatcher.make(config, queue, blocker, logger)
    } yield {
      //val loggingClient = ResponseLogger(logHeaders = true, logBody = true)(RequestLogger(logHeaders = true, logBody = true)(httpClient))
      val deepStackService: DeepStackService = new DeepStackServiceImpl(config, blocker, httpClient)
      val imageProcessor: ImageProcessor = new ImageProcessorImpl(config, deepStackService, logger)
      (imageProcessor, queue, logger)
    }
  }

  private def logCameras(cameras: List[CameraConfig], logger: Logger): IO[Unit] =
    cameras.map(c => logger.log(s"Camera ${c.name} relevant objects: ${c.relevantObjects.mkString(", ")}")).sequence_
}
