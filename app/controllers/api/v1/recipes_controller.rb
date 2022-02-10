class Api::V1::RecipesController < Api::V1::BaseController
    def index
        respond_with Recipe.all
    end
end
