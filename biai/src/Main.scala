import cats.effect.IOApp
import cats.effect.{ExitCode, IO}
import better.files.File
import io.methvin.better.files.RecursiveFileMonitor

object Main extends IOApp {
    override def run(args: List[String]): IO[ExitCode] = {
        val folder = File("/tmp/aiinput")
        val watcher = new RecursiveFileMonitor(folder) {
            override def onCreate(file: File, count: Int): Unit =
                println(file.path)
        }
        IO.pure(ExitCode.Success)
    }
}