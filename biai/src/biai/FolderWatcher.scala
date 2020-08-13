package biai

import better.files.File
import io.methvin.better.files.RecursiveFileMonitor
import fs2.concurrent.Queue
import cats.effect._

import scala.concurrent.ExecutionContext.Implicits.global
import cats.effect.Resource

object FolderWatcher {

  type Event = Either[Throwable, File]

  def make(config: AppConfig, queue: Queue[IO, Event], blocker: Blocker): Resource[IO, Unit] =
    Resource
      .make(
        makeMonitor(config, queue)
      )(monitor => IO(monitor.close()))
      .evalMap(monitor => IO(monitor.start()(blocker.blockingContext)))

  private def makeMonitor(config: AppConfig, queue: Queue[IO, Event]): IO[RecursiveFileMonitor] =
    IO {
      new RecursiveFileMonitor(File(config.targetFolder)) {
        override def onCreate(file: File, count: Int): Unit =
          queue.enqueue1(Right(file)).unsafeRunSync()

        override def onException(exception: Throwable): Unit = queue.enqueue1(Left(exception)).unsafeRunSync()
      }
    }

}
