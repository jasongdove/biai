package biai

import better.files.File
import io.methvin.better.files.RecursiveFileMonitor

import scala.concurrent.ExecutionContext.Implicits.global

trait FolderWatcher {
  def start(): Unit
}

class FolderWatcherImpl(config: AppConfig, imageProcessor: ImageProcessor, log: Logger) extends FolderWatcher {
  override def start(): Unit = {
    val watcher = new RecursiveFileMonitor(File(config.targetFolder)) {
      override def onCreate(file: File, count: Int): Unit = {
        val image = Image.load(file.name)
        val camera = image.flatMap(i => config.cameras.find(_.name == i.cameraName))
        val action = (image, camera) match {
          case (Some(image), Some(camera)) => imageProcessor.processImage(image, camera)
          case _                           => log.log(s"Could not match camera for image at ${file.name}")
        }

        action.attempt.unsafeRunSync()
      }
    }

    watcher.start()
  }
}
