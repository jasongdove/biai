package biai

import cats.effect.{Blocker, ExitCode, IO, IOApp}
import cats.implicits._
import pureconfig._
import pureconfig.module.catseffect.syntax._

object Main extends IOApp {
  override def run(args: List[String]): IO[ExitCode] = {
    val configFolder =
      sys.env.getOrElse("BIAI_CONFIG_FOLDER", s"${System.getProperty("user.home")}/.config/biai")

    val logger: Logger = LoggerImpl

    Blocker[IO].use(blocker => {
      for {
        config <- ConfigSource.file(s"$configFolder/biai.conf").loadF[IO, AppConfig](blocker)
        _ <- logCameras(config.cameras, logger)
        deepStackService: DeepStackService = new DeepStackServiceImpl(config)
        imageProcessor: ImageProcessor = new ImageProcessorImpl(deepStackService, logger)
        folderWatcher: FolderWatcher = new FolderWatcherImpl(config, imageProcessor, logger)
        _ <- IO(folderWatcher.start())
        _ <- IO.never
      } yield ExitCode.Success
    })
  }

  def logCameras(cameras: List[CameraConfig], logger: Logger): IO[Unit] =
    cameras.map(c => logger.log(s"Camera ${c.name} relevant objects: ${c.relevantObjects.mkString(", ")}")).sequence_
}
