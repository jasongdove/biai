package biai

import better.files.File
import cats.data.NonEmptyList
import cats.effect.IO
import cats.implicits._

trait ImageProcessor {
  def processImage(file: File): IO[Unit]
}

class ImageProcessorImpl(config: AppConfig, deepStackService: DeepStackService, logger: Logger) extends ImageProcessor {
  override def processImage(file: File): IO[Unit] =
    for {
      image <- Image.load(file)
      camera <- config.cameras.find(c => c.enabled && c.name == image.cameraName).liftTo[IO](CameraNotFound(image.file))
      _ <- logger.log(s"Image at ${image.file.canonicalPath} was created and matched to camera ${camera.name}")
      // TODO: check cooldown(s)
      response <- deepStackService.detectObjects(image)
      objects <- getRelevantObjects(camera, response)
      _ <- logRelevantObjects(objects)
      // TODO: process telegram trigger
      // TODO: process generic trigger(s)
    } yield ()

  private def getRelevantObjects(camera: CameraConfig, response: DeepStackResponse): IO[NonEmptyList[DeepStackObject]] =
    if (response.success)
      response.predictions
        .filter { o =>
          val isRelevant = camera.relevantObjects.contains(o.label)
          val confidence = o.confidence * 100
          isRelevant && confidence >= camera.minConfidence && confidence <= camera.maxConfidence
        }
        .toNel
        .liftTo[IO](NoRelevantObjectsDetected)
    else IO.raiseError(NoObjectsDetected)

  private def logRelevantObjects(objects: NonEmptyList[DeepStackObject]): IO[Unit] = {
    logger.log(s"Detected relevant objects ${objects.map(o => (o.label, o.confidence * 100))}")
  }
}
