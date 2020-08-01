package biai

import better.files.File
import io.methvin.better.files.RecursiveFileMonitor

import scala.concurrent.ExecutionContext.Implicits.global

trait FolderWatcher {
  def watch(folder: String, callback: String => Unit)
}

object FolderWatcherImpl extends FolderWatcher {
  override def watch(folder: String, callback: String => Unit): Unit = {
    val watcher = new RecursiveFileMonitor(File(folder)) {
      override def onCreate(file: File, count: Int): Unit = {
        callback(file.path.toString)
      }
    }
    watcher.start()
  }
}