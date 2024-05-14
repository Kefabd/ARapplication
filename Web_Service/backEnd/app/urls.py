from django.urls import path

from . import views

urlpatterns = [
    path("", views.index, name="index"),
    path("upload-image/", views.handle_image_upload)
]