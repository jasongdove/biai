package biai

import cats.effect.IO

trait ImageProcessor {
  def processImage(image: Image, camera: CameraConfig): IO[Unit]
}

class ImageProcessorImpl(deepStackService: DeepStackService, logger: Logger) extends ImageProcessor {
  override def processImage(image: Image, camera: CameraConfig): IO[Unit] = {
    for {
      _ <- logger.log(s"Image at ${image.fileName} was created and matched to camera ${camera.name}")
      _ <- deepStackService.detectObjects(image)
    } yield ()
  }
}