package biai

import cats.effect.IO

trait Logger {
  def log(message: String): IO[Unit]
}

object LoggerImpl extends Logger {
  override def log(message: String): IO[Unit] = {
    println(message)
    IO.unit
  }
}