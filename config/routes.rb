Rails.application.routes.draw do
  # Define your application routes per the DSL in https://guides.rubyonrails.org/routing.html

  root "index#main"
  
  namespace :api do
    namespace :v1 do
      resources :recipes, only: [:index] #, :create, :destroy, :update]
      resources :categories, only: [:index]
    end
  end

  get "*path", to: "index#main"
end
