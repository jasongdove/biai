package biai

import better.files.File
import cats.effect._
import fs2.concurrent.Queue
import io.methvin.better.files.RecursiveFileMonitor

object FolderWatcher {

  type Event = Either[Throwable, File]

  def make(config: AppConfig, queue: Queue[IO, Event], blocker: Blocker, logger: Logger): Resource[IO, Unit] =
    Resource
      .make(
        makeMonitor(config, queue)
      )(monitor => IO(monitor.close()))
      .evalMap(startMonitor(_, blocker, logger))

  private def makeMonitor(config: AppConfig, queue: Queue[IO, Event]): IO[RecursiveFileMonitor] =
    IO {
      new RecursiveFileMonitor(File(config.targetFolder)) {
        override def onCreate(file: File, count: Int): Unit =
          queue.enqueue1(Right(file)).unsafeRunSync()

        override def onException(exception: Throwable): Unit = queue.enqueue1(Left(exception)).unsafeRunSync()
      }
    }

  private def startMonitor(monitor: RecursiveFileMonitor, blocker: Blocker, logger: Logger): IO[Unit] =
    for {
      _ <- IO(monitor.start()(blocker.blockingContext))
      _ <- logger.log(s"Watching folder ${monitor.root.canonicalPath} for images")
    } yield ()
}
