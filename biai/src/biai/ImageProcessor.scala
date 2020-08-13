package biai

import better.files.File
import cats.effect.IO
import cats.implicits._

trait ImageProcessor {
  def processImage(file: File): IO[Unit]
}

class ImageProcessorImpl(config: AppConfig, deepStackService: DeepStackService, logger: Logger) extends ImageProcessor {
  override def processImage(file: File): IO[Unit] =
    for {
      image <- Image.load(file)
      camera <- config.cameras.find(_.name == image.cameraName).liftTo[IO](CameraNotFound(image.file))
      _ <- logger.log(s"Image at ${image.file.name} was created and matched to camera ${camera.name}")
      r <- deepStackService.detectObjects(image)
      _ <- logger.log(r.toString)
    } yield ()
}

case class CameraNotFound(file: File) extends Throwable {
  override def getMessage: String = s"Could not match camera for image at ${file.canonicalPath}"
}
