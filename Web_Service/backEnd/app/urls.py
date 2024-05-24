from django.urls import path

from . import views

urlpatterns = [
    path("", views.index, name="index"),
    path("frames/", views.handle_frames),
    path("image_target/", views.handle_image_target)
]