package biai

import cats.effect.IO

trait DeepStackService {
  def detectObjects(image: Image): IO[Unit]
}

class DeepStackServiceImpl(config: AppConfig) extends DeepStackService {
  override def detectObjects(image: Image): IO[Unit] = {
    // TODO: multipart form post
    IO.unit
  }
}